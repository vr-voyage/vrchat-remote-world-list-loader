# Remote World List Loader for VRChat

A simple example of how to use the new VRChat String Loader to
load a World list formated as a TSV from a website (pastebin.com here),
and display it as a list, with pagination and portal opening when
clicking on a world name.

# Requirements

* VRChat Creator Companion
* World SDK >= 3.1.11
* UdonSharp >= 1.1.7

# Format of the world list

The world list is supposed to be formatted as "Tab Separated Values".
This is the easiest format to parse. Just make sure you don't have Tab
characters in the world names (and if you do... Are you OK ?).

You can configure the script and tell it on which field it should sample
the various elements (Currently World ID, Title and Author).

[The example](https://pastebin.com/raw/GLDwWZja) uses pastebin.com since
it's [whitelisted by VRChat](https://docs.vrchat.com/docs/string-loading).
You can use any hosting service, but anything beside pastebin.com,
Github Gists and Github pages will require the user to accept "Untrusted URLS".

For the moment, only the prefab works. Just grab it from :
* **Packages** > **Voyage's Simple Scripts : Remote World List Loader** > **Runtime** > **RemoteWorldListLoader**

# Licenses

Currently, the prefab uses :

* [Noto Sans CJK Font](https://github.com/notofonts/noto-cjk/releases/tag/Sans2.004)
* [Kenney Game Icons](https://kenney.nl/assets/game-icons)
