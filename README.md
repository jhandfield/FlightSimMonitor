# FlightSimMonitor
[![.NET Framework](https://github.com/jhandfield/FlightSimMonitor/actions/workflows/NETFramework.yaml/badge.svg)](https://github.com/jhandfield/FlightSimMonitor/actions/workflows/NETFramework.yaml)

A .NET Framework library to simplify monitoring of flight telemetry and events in MSFS 2020. The goal ultimately is for this library to be able to serve as intermediary between FS2020 and another application, like FSEconomy or NeoFly or OnAir, allowing for these applications to be written around standard .NET concepts without having to delve into SimConnect to get the data they need.

## Current Status
This project is actively in development - as of 3/1/2021, the library is capable of connecting to SimConnect and firing events on connect and disconnect, but little else.

## Roadmap
Updated 3/1/2020
* Implement simple data retrieval and monitoring from SimConnect via events
* Implement remote shipping of data and events
* ???
* PROFIT
