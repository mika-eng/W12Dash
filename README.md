# W12Dash
Dashboard for the iracing Mercedes-AMG W12 E-Performance

Easy-to-use Dashboard for the Mercedes-AMG W12 E-Performance F1 car using the iracing telemetry.
Written in C# using .NET Framework WPF.

W12Dash.exe v1.1.0.0
--------------------

New pop-up overlays when chaning:
- brake bias
- brake migration
- diff entry
- diff middle
- diff exit
- engine braking


W12Dash.exe v1.0.0.0
--------------------

Tested with:
- Windows 10 Home 20H2
- iracing 2022 Season 1 Hotfix 2 Release (windowed mode)

Green lights (left upper side):
- DRS not available (all lights off)
- DRS coming up (1 light)
- DRS available (2 lights)
- DRS enabled (4 lights)

Red and blue lights:
Engine RPM, depending on current gear

RACE page:
- remaining deploy mode changes
- completed laps
- delta to session best lap
- pit limiter indicator
- current gear
- deploy mode (no deploy, qualy, attack, balanced, build)
- remaining Battery
- brake bias
- last lap fuel usage in kg
- target fuel usage for each lap to finish the race (not tested in race)
- laptime last lap

How to use:
Simply start the W12Dash.exe.
It automatically minimizes when leaving the car in iracing and goes back to normal when entering the car.
Changing scale: s + up/down-key
Changing opacity: o + up/down-key
