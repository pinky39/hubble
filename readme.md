# Hubble

Project done for a coding interview.

## Description
We run a bunch of services as Docker containers. They talk to each other via internal network, some of them exposing their interfaces to outside world through a reverse proxy. We need a health-check system that some external service (such as Google Stackdriver) will be able to poll for the health of our ecosystem. We could in theory let Stackdriver call our services directly, but this means that we would need to expose them to the outside world [for Stackdriver to be able to reach them]. The health-check endpoints will also be DOS targets, so it is best if we develop a dedicated service that will handle all our health-polling requests.

Your task is to implement a .NET Core MVC service for monitoring our ecosystem of services. Let's call it Hubble.