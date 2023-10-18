![Banner](Images/Banner.png)

# Sholo.ActivityLevels

[![Twitter URL](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)](https://twitter.com/scottt732)
[![Twitter Follow](https://img.shields.io/twitter/follow/scottt732.svg?style=social&label=Follow)](https://twitter.com/scottt732)

ActivityLevels monitors `binary_sensors` in [Home Assistant](https://www.home-assistant.io/) for state changes and creates new sensors 
via MQTT discovery that indicate the activity levels in areas of your home on a scale of 0.0 to 5.0 (by default).  Each triggered
`binary_sensor` increases the value of parent `sensor`s and they cool down gradually over time toward 0.0 again (idle).

This allows you to build smarter automations in [Home Assistant](https://www.home-assistant.io/), 
[AppDaemon](https://appdaemon.readthedocs.io/en/latest/), [Sholo.HomeAssistant](https://github.com/scottt732/Sholo.HomeAssistant), 
etc. that are aware of the activity levels around your home are.  

- When all of the `binary_sensors` in the room cause the `sensor` that tracks them to drop to 0.0, you can have higher 
  confidence that turning lights off, locking doors, etc. will not disrupt anyone vs. acting on the `binary_sensor`s alone.  
- When you would typically put your home in night time mode, maybe you'd want to make an exception if the activity level
  of your entire home is very high (looks like you're having a party).
- When the activity level in your house goes above 0.0 and everyone is away, send a push notification.  Maybe there's a 
  security event you'd want to know about.

## How It Works

Imagine a 2 story house with 2 bedrooms, a living room and a kitchen.  There are 4 exterior windows and 2
exterior doors.

- **House** --> `sensor.house_activity_level`
  - **Floor 1** --> `sensor.floor_1_activity_level`
    - **Living Room** --> `sensor.living_room_activity_level`
      . `binary_sensor.living_room_motion`
      . `binary_sensor.living_room_windows`
      . `binary_sensor.front_door`
    - **Kitchen** --> `sensor.kitchen_activity_level`
      . `binary_sensor.kitchen_motion`
  - **Floor 2** --> `sensor.floor_2_activity_level`
    - **Master Bedroom** --> `sensor.master_bedroom_activity_level`
      . `binary_sensor.balcony_door`
      . `binary_sensor.master_bedroom_motion`
    - **Guest Room** --> `sensor.guest_room_activity_level`
      . `binary_sensor.guest_room_motion`

Here's an example exchange.  Assume all sensors start at 0.0 (nobody's home):

- Someone opens the front door
  - `binary_sensor.front_door` triggers
  - `sensor.living_room_activity_level` increases from 0.0 to 1.0
  - `sensor.floor_1_activity_level` increases from 0.0 to 1.0
  - `sensor.house_activity_level` increases from 0.0 to 1.0
- A few seconds later, the visitor triggers the living room motion detector.
  - `binary_sensor.living_room_motion` triggers
  - `sensor.living_room_activity_level` increases from 1.0 to 2.0
  - `sensor.floor_1_activity_level` increases from 1.0 to 2.0
  - `sensor.house_activity_level` increases from 1.0 to 2.0

The cooldown schedule for the sensors determines how long it would take for the sensor to go from 5.0 to 0.0.  In other 
words, it's the slope (m) of the line.  On each sensor value increase, the y-offset changes (b) but the slope remains 
constant.  For a sensor with a 60 minute cooldown from 5.0 to 0.0, each integer drop will take 12 minutes.  So in this
example, the `sensor.house_activity_level` will drop from 2.0 to 1.0 after 12 minutes.  Then from 1.0 to 0.0 12 minutes
later (assuming no other activity occurs on the sensors below it).  

The `sensor.living_room_activity_level` sensor is configured with a 30 minute cooldown schedule (6 minutes for a 1.0 
drop).  So 12 minutes later, while `sensor.house_activity_level` and `sensor.floor_1_activity_level` will be at 1.0 
(down from 2.0), `sensor.living_room_activity_level` will be at 0.

## Related Projects

ActivityLevels is built on top of many of the libraries below.

<table width="100%">
    <tr>
        <th colspan="4">Home Automation Foundation Libraries</th>
    </tr>
    <tr>
        <th width="25%"><a href="https://github.com/scottt732/Sholo.CommandLine">Sholo.CommandLine</a></td>
        <th width="25%"><a href="https://github.com/scottt732/Sholo.HomeAssistant">Sholo.HomeAssistant</a></td>
        <th width="25%"><a href="https://github.com/scottt732/Sholo.Mqtt">Sholo.Mqtt</a></td>
        <th width="25%"><a href="https://github.com/scottt732/Sholo.Utils">Sholo.Utils</a></td>
    </tr>
    <tr>
        <th width="25%"><a href="https://github.com/natemcmaster/CommandLineUtils">natemcmaster/CommandLineUtils</a></td>
        <th width="25%">-</td>
        <th width="25%"><a href="https://github.com/chkr1011/MQTTnet">chkr1011/MQTTnet</a></td>
        <th width="25%">-</td>
    </tr>
</table>

## License

MIT License

Copyright (c) 2020 Scott Holodak

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
