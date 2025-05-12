
##### Defines simulated environment

class PhysicalEnvironment:
    def __init__(self):
        self.things = {
            "agent": EmbodiedAgent(),
            "door": Door(),
            "light": Light(),
        }
    
    def get_things(self):
        return self.things.keys()
    
    def get_thing(self, name):
        return self.things.get(name)

class EmbodiedAgent:
    def __init__(self):
        self.position = (0, 0)

    def get_position(self):
        return self.position

    def set_position(self, position):
        self.position = position

    def move(self, dx, dy):
        x, y = self.position
        self.position = (x + dx, y + dy)

class Door:
    def __init__(self):
        self.is_open = False
        self.position = (2, 4)

    def open(self):
        self.is_open = True

    def close(self):
        self.is_open = False

    def is_open(self):
        return self.is_open

class Light:
    def __init__(self):
        self.is_on = False
        self.position = (0, 0)

    def turn_on(self):
        self.is_on = True

    def turn_off(self):
        self.is_on = False

    def is_on(self):
        return self.is_on
    
simulation = PhysicalEnvironment()
you = simulation.get_thing("agent")