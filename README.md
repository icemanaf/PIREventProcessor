# PIREventProcessor
.NET core based project which processes Kafka based messages
[![Build Status](https://travis-ci.com/icemanaf/PIREventProcessor.svg?branch=master)](https://travis-ci.com/icemanaf/PIREventProcessor)

## What is it?
A .Net core based event sink which implements filters which listen to specific event types (currently looks at Kafka Topics) and does further processing and/or initiates actions , examples could be listening into a stream of messages coming through a set of PIR sensors , either setting and alarm and prompting recording of a nearby camera.

This image is specifically targeted at Raspberry PI 3's and up using Arm32 base images.

Get the image by running 
```
docker pull icemanaf/rpi-eventprocessor:latest
```
Run it interactively using
```
docker run -it  icemanaf/rpi-eventprocessor:latest
```
## This is still work in progress.
Todo - need to handle configuration , by mapping the appsettings.json on a volume. 
