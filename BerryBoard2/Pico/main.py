from machine import Pin
import utime

class BlinkingLED:
    def __init__(self, pin, blink_interval):
        self.led = Pin(pin, Pin.OUT)
        self.blink_interval = blink_interval
        self.blink_timer = blink_interval

    def update(self, interval):
        self.blink_timer -= interval
        if self.blink_timer <= 0.0:
            self.led.toggle()
            self.blink_timer = self.blink_interval

class ButtonGrid:
    def __init__(self, column_pins, row_pins):
        self.columns = [Pin(pin, Pin.OUT) for pin in column_pins]
        self.rows = [Pin(pin, Pin.IN, Pin.PULL_DOWN) for pin in row_pins]
        self.button_count = len(self.columns) * len(self.rows)
        self.buttons = [False] * self.button_count
        self.history = [False] * self.button_count

    def update(self):
        for c, column in enumerate(self.columns):
            column.value(1)

            for r, row in enumerate(self.rows):
                button_index = r * len(self.columns) + c
                self.buttons[button_index] = row.value() == 1

            column.value(0)

        for i in range(self.button_count):
            if self.buttons[i] != self.history[i]:
                if self.buttons[i]:
                    print(i)
                self.history[i] = self.buttons[i]

class RotaryEncoder:
    def __init__(self, clk_pin, dt_pin):
        self.clk = Pin(clk_pin, Pin.IN, Pin.PULL_UP)
        self.dt = Pin(dt_pin, Pin.IN, Pin.PULL_UP)
        self.counter = 0
        self.clkLastState = self.clk.value()

    def update(self):
        clkState = self.clk.value()
        if clkState != self.clkLastState:
            if self.dt.value() != clkState:
                self.counter += 1
            else:
                self.counter -= 1
            print('Rotary Counter:', self.counter)
        self.clkLastState = clkState

# Define pins for buttons
column_pins = [6, 7, 8, 9]
row_pins = [20, 19, 18]
button_grid = ButtonGrid(column_pins, row_pins)

# Define rotary encoders
rotary1 = RotaryEncoder(1, 2)
rotary2 = RotaryEncoder(3, 4)

# Define LED
led = BlinkingLED(25, 1.0)

interval = 0.01
while True:
    button_grid.update()
    rotary1.update()
    rotary2.update()
    led.update(interval)

    utime.sleep(interval)