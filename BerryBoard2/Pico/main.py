from machine import Pin, UART
import utime
import sys
import select

# Define the pins for the columns and rows
column_pins = [6, 7, 8, 9]
row_pins = [20, 19, 18]
led = Pin(25, Pin.OUT)

# Create Pin objects for each pin, columns as output and rows as input with pull-down
columns = [Pin(pin, Pin.OUT) for pin in column_pins]
rows = [Pin(pin, Pin.IN, Pin.PULL_DOWN) for pin in row_pins]

button_count = len(columns) * len(rows)
buttons = [False] * button_count
history = [False] * button_count

blink_interval = 1
blink_timer = blink_interval
interval = 0.01
while True:
    for c, column in enumerate(columns):
        column.value(1)
        
        for r, row in enumerate(rows):
            button_index = r * len(columns) + c
            buttons[button_index] = row.value() == 1

        column.value(0)
    
    for i in range(button_count):
        if buttons[i] != history[i]:
            if not buttons[i]:
                print(i)
                #print(f"Button {i + 1} was released")
            #else:
                #print(f"Button {i + 1} was pressed")
            history[i] = buttons[i]
            
        # Handle LED blinking
    blink_timer -= interval
    if (blink_timer <= 0.0):
        led.toggle()
        blink_timer = blink_interval
    
    utime.sleep(interval)
    
"""
# Define pins
output_pin = Pin(9, Pin.OUT)
input_pin = Pin(20, Pin.IN, Pin.PULL_DOWN)

# Set the output pin to high
output_pin.value(1)

while True:
    print(input_pin.value())
    utime.sleep(1)
"""
