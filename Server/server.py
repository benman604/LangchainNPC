#!/usr/bin/env python

from typing import Any, List, Union
import logging
import uvicorn
from fastapi import FastAPI, HTTPException
from langchain.agents import AgentExecutor
from langchain.agents.format_scratchpad.openai_tools import (
    format_to_openai_tool_messages,
)
from langchain.agents.output_parsers.openai_tools import OpenAIToolsAgentOutputParser
from langchain_core.messages import AIMessage, FunctionMessage, HumanMessage
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder, FewShotPromptTemplate
from langchain_core.tools import tool
from langchain_core.utils.function_calling import format_tool_to_openai_tool
# from langchain_openai import ChatOpenAI
from pydantic import BaseModel, Field
from langserve import add_routes
from tools import tools

# import preprompt message
preprompt_tuples = []
with open("preprompt.csv") as f:
    for line in f:
        line = line.strip()
        if not line:
            continue 
        role, content = line.split(",", 1) 
        preprompt_tuples.append((role.strip(), content.strip()))

prompt = ChatPromptTemplate.from_messages(
    preprompt_tuples + [
        # Please note the ordering of the fields in the prompt!
        # The correct ordering is:
        # 1. history - the past messages between the user and the agent
        # 2. user - the user's current input
        # 3. agent_scratchpad - the agent's working space for thinking and
        #    invoking tools to respond to the user's input.
        # If you change the ordering, the agent will not work correctly since
        # the messages will be shown to the underlying LLM in the wrong order.
        MessagesPlaceholder(variable_name="chat_history"),
        ("user", "{input}"),
        MessagesPlaceholder(variable_name="agent_scratchpad"),
    ]
)


# We need to set streaming=True on the LLM to support streaming individual tokens.
# Tokens will be available when using the stream_log / stream events endpoints,
# but not when using the stream endpoint since the stream implementation for agent
# streams action observation pairs not individual tokens.
# See the client notebook that shows how to use the stream events endpoint.
# llm = ChatOpenAI(model="gpt-3.5-turbo", temperature=0, streaming=True)

import os
import dotenv
dotenv.load_dotenv(".env")
from langchain_groq import ChatGroq
API = os.getenv("GROQ_API_KEY")
MODEL = 'gemma2-9b-it'
llm = ChatGroq(
    model_name=MODEL,
    temperature=0.7,
    api_key=API,
    streaming=False,
)

# tools = [word_length, turn_on_lights, turn_off_lights, lights_status, half_number, get_all_primes_less_than]


llm_with_tools = llm.bind_tools(tools)

agent = (
    {
        "input": lambda x: x["input"],
        "agent_scratchpad": lambda x: format_to_openai_tool_messages(
            x["intermediate_steps"]
        ),
        "chat_history": lambda x: x["chat_history"],
    }
    | prompt
    # | prompt_trimmer # See comment above.
    | llm_with_tools
    | OpenAIToolsAgentOutputParser()
)
agent_executor = AgentExecutor(agent=agent, tools=tools, verbose=False, return_intermediate_steps=True)


app = FastAPI(
    title="LangChain Server",
    version="1.0",
    description="Spin up a simple api server using LangChain's Runnable interfaces",
)

# We need to add these input/output schemas because the current AgentExecutor
# is lacking in schemas.
class Input(BaseModel):
    input: str
    # The field extra defines a chat widget.
    # Please see documentation about widgets in the main README.
    # The widget is used in the playground.
    # Keep in mind that playground support for agents is not great at the moment.
    # To get a better experience, you'll need to customize the streaming output
    # for now.
    chat_history: List[Union[HumanMessage, AIMessage, FunctionMessage]] = Field(
        ...,
        extra={"widget": {"type": "chat", "input": "input", "output": "output"}},
    )

class Output(BaseModel):
    output: Any


# Adds routes to the app for using the chain under:
# /invoke
# /batch
# /stream
# /stream_events
# add_routes(
#     app,
#     agent_executor.with_types(input_type=Input, output_type=Output).with_config(
#         {"run_name": "agent"}
#     ),
# )

@app.post("/invoke", response_model=Output)
async def invoke_agent(input_data: Input):
    try:
        print(f"Received request: {input_data.dict()}")

        # Call the agent with user input and chat history
        response = agent_executor.invoke({"input": input_data.input, "chat_history": input_data.chat_history})

        print(f"Agent Response: {response}")

        # Extract response components
        # output_text = response.get("output", "No response.")
        # metadata = {
        #     "run_id": response.get("run_id", ""),
        #     "intermediate_steps": response.get("intermediate_steps", []),
        #     "tool_calls": response.get("tool_calls", [])
        # }

        return Output(output=response)

    except Exception as e:
        logging.error(f"Error in agent execution: {e}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    uvicorn.run(app, host="localhost", port=8000)