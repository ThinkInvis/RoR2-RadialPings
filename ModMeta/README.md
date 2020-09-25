# Radial Pings

## Description

No mic? No problem! This mod adds a way to control exactly what you say with a ping.

**To use:**

- Press *and hold* your ping keybind to bring up a radial menu.
- Move the cursor, then release the ping keybind to confirm an option.
- Some options, indicated by a filling progress bar and a "..." in the name, can activate after hovering over them for some time.

Press and release the ping keybind within a configurable time (default 0.2s) to perform a vanilla Quick Ping instead of opening the menu.

Some ping types start a limited-time vote. The Yes/No ping types are used to vote on votable pings.

### Current options

#### Quick Ping

Uses vanilla ping behavior. Occupies the menu's inner dead zone (keep the cursor inside all buttons).

#### Cancel

Performs no ping at all. Occupies the menu's outer dead zone (move the cursor outside of the menu).

#### Let's Go!

Generally aggressive option.

- Ping an item/interactable to claim it.
- Ping the Teleporter to suggest starting it (votable).
- Ping an enemy to suggest attacking it.
- Ping any other point to say you want to hurry up (votable).

#### Look at This

An alternative for Move Here with less urgency. Also useful for pointing out lore.

- Ping an item/interactable/player/enemy/etc. to ping it by name.
- Ping any other point for a position-only ping.

#### Move Here

A variant of the vanilla ping which can never ping an object, only a surface.

- Ping any point for a position-only ping.

#### Yes/No

Pings yourself and says "yes"/"no" in chat, complete with matching in-world icon.

- Ping any point for a self-ping.
- Hover to open a menu for voting on votable pings.

#### Help Me!

Pings yourself for an extended period of time, with a blaring red icon and urgent message in chat.

- Ping any point for a self-ping.

#### Ping Player...

ON HOVER: Opens a second menu with one option for each player in your current game.

- Ping an item to suggest that the player should take it.
- Ping an interactable to suggest that the player should use it.
- Ping the Teleporter to tell the player to get there.
- Ping an enemy to tell the player to focus attacks on that enemy.
- Ping an ally to tell the player to follow that ally.
- Ping any other point to poke the player.
- Select the inner or outer dead zone to cancel pinging.

## Issues/TODO

- An Order Drones feature is planned.
- Streamlined support for other mods to add additional options is planned.
- Let's Go! options leave some cases (namely targeting allies) poorly handled.
- Some other options may not be final.
- Text in the UI is a little shakily positioned.
- See the GitHub repo for more!

## Changelog

**2.0.0**

- Finished implementing Ping Player options.
- Slightly improved behavior of inner dead zone and button selection.
- Added an experimental Respondables system. Let's Go! > Start Teleporter and Let's Go! > Hurry Up pings can now be responded to with a vote within 30 seconds of the original ping.
- Reorganized project into 3 modules (RadialMenu, PingCatalog, RadialPingsCore). RadialMenu may be split into its own mod at a later date.
- Added documentation in several areas.

**1.0.0**

- Initial release. Adds a radial menu on holding Ping with 7 options (Let's Go!, Look at This, Move Here, Yes, No, Help Me!, Ping Player).