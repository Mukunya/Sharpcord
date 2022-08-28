# Sharpcord
Another c# discord api implementation. Featuring a host-module based architecture, so you can run multiple bots easily

# Getting started
Download a build of the host from [Releases](https://github.com/Mukunya/Sharpcord/releases) or build it yourself, that will be the program that runs your bots.

Also download the Sharpcord bot library, add it as a reference to a new __.NET 6 class library project__ this will be your bot. Refer to the example bot for further instructions.
Place all the bot dlls and dlls of any libraries you use in your bot in the /modules folder next to the host

Run the host, it will say if your bot was loaded or not in the console and logs. Run it with the -D flag for more detailed logging.
