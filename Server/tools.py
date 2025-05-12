import json
from typing import Optional
from langchain_core.tools import tool
from langchain_core.messages import ToolMessage

# Import the simulation and agent from the environment module
from environment import simulation, you

@tool
def turn_lights_on() -> str:
    """Turns the light on if the agent is at the correct position and light is off."""
    agent_position = you.get_position()
    light = simulation.get_thing("light")

    # Check if the agent is at the light's position
    if agent_position != light.position:
        return f"world>> Error: You are at {agent_position}, lights are at {light.position}. Please move closer."
    
    # Check if the light is already on
    if light.is_on:
        return "world>> Error: Lights are already on."
    
    light.turn_on()
    return "world>> Lights are now on."

@tool
def turn_lights_off() -> str:
    """Turns the light off if the agent is at the correct position and light is on."""
    agent_position = you.get_position()
    light = simulation.get_thing("light")

    # Check if the agent is at the light's position
    if agent_position != light.position:
        return f"world>> Error: You are at {agent_position}, lights are at {light.position}. Please move closer."
    
    # Check if the light is already off
    if not light.is_on:
        return "world>> Error: Lights are already off."
    
    light.turn_off()
    return "world>> Lights are now off."

@tool
def open_door() -> str:
    """Opens the door if the agent is at the correct position."""
    agent_position = you.get_position()
    door = simulation.get_thing("door")

    # Check if the agent is at the door's position
    if agent_position != door.position:
        return f"world>> Error: You are at {agent_position}, door is at {door.position}. Please move closer."

    # Open the door
    door.open()
    return "world>> The door is now open."

@tool
def close_door() -> str:
    """Closes the door if the agent is at the correct position."""
    agent_position = you.get_position()
    door = simulation.get_thing("door")

    # Check if the agent is at the door's position
    if agent_position != door.position:
        return f"world>> Error: You are at {agent_position}, door is at {door.position}. Please move closer."

    # Close the door
    door.close()
    return "world>> The door is now closed."

@tool
def move_self(dx: float, dy: float) -> str:
    """Moves you by the specified dx and dy."""
    agent_position = you.get_position()
    new_position = (agent_position[0] + dx, agent_position[1] + dy)
    
    # Update the agent's position
    you.set_position(new_position)
    
    return f"world>> Moved to {new_position}."

@tool
def get_environment_state() -> str:
    """Returns the current state of the environment."""
    agent_position = you.get_position()
    door_state = "open" if simulation.get_thing("door").is_open else "closed"
    light_state = "on" if simulation.get_thing("light").is_on else "off"
    
    return f"world>> You are at position {agent_position}. The door is {door_state}. The light is {light_state}."


# Register the tools
tools = [
    turn_lights_on,
    turn_lights_off,
    open_door,
    close_door,
    move_self,
    get_environment_state
]