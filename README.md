This is ZTransport, a mod for Oxygen Not Included. ZTransport allows multiple Oxygen Not Included colonies to exchange energy, gases, and liquids, as though they were different Z-slices of the same asteroid. This adds some interesting teamwork opportunities to the game.

This mod was produced as part of an apprenticeship. The bulk of the client's code was written by BloodyRum, under the oppressive and pedantic gaze of Solra Bizna (who also wrote this readme).

# Installation

Place the ZTransport folder into Oxygen Not Included's Mods folder, or install it from the Steam Workshop.

One of the players must run a server in order for the mod to be useful. The server is not included. The server is found here: <https://github.com/SolraBizna/onizd>

# Features

- Transport gases, liquids, and energy between colonies running at the same time on different computers
- All elements can be transmitted
- Temperature, mass, and even germs are preserved

# Usage

Let us use a simple example. Alice has long ago created a sustainable oxygen system, and has much more oxygen production than she needs. Bob, however, has been having trouble with his algae supply; in fact, it has just run out, bringing his oxygen generation to a screeching halt. His Duplicants only have a few cycles to live. What can he do!?

Well, he can take some of Alice's oxygen, of course.

First, Alice and Bob must both be connected to the same server. Say Alice is the one running [`onizd`](https://github.com/SolraBizna/onizd) and her IP address is `192.0.2.55`. Both Alice and Bob can click on their **Printing Pod** (or any **Z** building), click on the "ZTransport" button, type `192.0.2.55` in the Server Address field, and click "Confirm". They will now be connected to the same server. The setting will persist with the world, so they **only need to do this step once per world**. (If Alice and Bob are on different networks, figuring out what IP address to use will be a fun task, way beyond the scope of this readme... but it may help them to know that the default port is 5496, and the protocol uses TCP, not UDP.)

Now, Alice must build a **Z Gas Sender**, and Bob must build a **Z Gas Receiver**, both at the same coordinates. This Sender will send gas to the Receiver, acting something like a **Gas Bridge** with one end in Alice's asteroid and the other in Bob's. (Once a **Z** building has been constructed, it will show its coordinates when either selected or moused over; positive X goes to the right, and positive Y goes up. It's clumsy, but this lets you figure out where you are relative to one another, and with patience and precision, get everything linked up.)

Once these buildings are constructed, Alice must run a Gas Pipe from her oxygen generation system to the **Z Gas Sender**, and Bob must run Gas Pipe from his **Z Gas Receiver** to a **Gas Vent** somewhere in his base. Once this is completed, oxygen should begin flowing from one colony to another. Bob is saved! Hooray! (Until Alice's water runs out from all the extra load on her SPOM...)

# Missing

These features are **not yet present**, but are planned to be added some day:

- Transporting solids along conveyors
- Transporting solids through "drop boxes"
- **Z Power Sender** working more like a Transformer than a Battery (and multiple versions of it at different power ratings)
- "Ghost" images of **Z** buildings in other colonies that don't yet have a counterpart in the current colony (to make lining things up easier)
- Better error messages
- Integrated server
- Optional game speed syncing, for players who would rather annoy each other than *slightly* cheat time in a game whose plot revolves around a giant apocalyptic time anomaly...

# Legalese

ZTransport is copyright ©2020 BloodyRum. If you submit improvements to ZTransport in the form of Pull Requests via GitHub, it is assumed that you are assigning copyright on your improvements to BloodyRum, unless you clearly and explicitly state otherwise *before* your Pull Request is merged.

ZTransport is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

ZTransport is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received [a copy of the GNU General Public License](COPYING.md) along with ZTransport. If not, see <https://www.gnu.org/licenses/>.

ZTransport contains small portions of code derived from a decompiled version of Oxygen Not Included. These portions are believed to be necessary to interact properly with Oxygen Not Included's systems and/or with other mods. Inclusion of these portions is believed to be consistent with the spirit of Oxygen Not Included modding, and intended neither as infringement upon nor usurping of Klei Entertainment's copyrights.
