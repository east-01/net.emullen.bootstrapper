# Project Name

## Table of Contents
- [Introduction](#introduction)
- [Getting Started](#getting-started)
- [Usage](#usage)

---

## Introduction
If you need a system to initialize certain objects (your network manager for instance), you can
use a bootstrapper scene to accomplish this. 

## Getting Started
A bootstrapper is usually a prefab that goes in a scene. It has a set boostrap sequence on it
that will run when the scene is loaded. Once a bootstrap sequence is loaded by the first
bootstrapper that sequence must finish, after that a new bootstrapper can start a different 
sequence.<br>
<br>
__Only one bootstrapper can exist per scene.__<br>
<br>
There are two types of scenes with bootstrappers in them, the first is a true __bootstrapping
scene__ that actually runs bootstrap components while the other is a scene that points to a
specific bootstrapping scene (or multiple scenes) with a bootstrap sequence.<br>
<br>
An example to better illustrate this, we have two scenes in our game:<br>
&nbsp;&nbsp;&nbsp;&nbsp;*BootstrapScene  (Bootstrapping scene)*<br>
&nbsp;&nbsp;&nbsp;&nbsp;*GameplayScene   (Non-bootstrapping scene)*<br>
In each scene is a bootstrapper prefab, each type represented above. The GameplayScene will have a
bootstrap sequence on it that looks like this:
&nbsp;&nbsp;&nbsp;&nbsp;*BootstrapScene -> GameplayScene*<br>
Pretty simple, but we can add more flexibility. Lets say our game is networked, and we're required
to be in a lobby before we can enter gameplay. We'll have a scene structure like this:<br>
&nbsp;&nbsp;&nbsp;&nbsp;*BootstrapScene  (Bootstrapping scene)*<br>
&nbsp;&nbsp;&nbsp;&nbsp;*LobbyBootstrapScene  (Bootstrapping scene)*<br>
&nbsp;&nbsp;&nbsp;&nbsp;*GameplayScene   (Non-bootstrapping scene)*<br>
&nbsp;&nbsp;&nbsp;&nbsp;*TitleScene  (Non-bootstrapping scene)*<br>
We have a couple paths for the game to to take now, starting either at the TitleScene or the
GameplayScene. Each scene's bootstrap sequence (on the bootstrapper object) would look something 
like this:<br>
&nbsp;&nbsp;&nbsp;&nbsp;*TitleScene: BootstrapScene -> TitleScene*<br>
&nbsp;&nbsp;&nbsp;&nbsp;*GameplayScene: BootstrapScene -> LobbyBootstrapScene -> GameplayScene*<br>
Notice how the GameplayScene requires the LobbyBootstrapScene before it can get to the 
GameplayScene to satisfy the network requirements.<br>
However, this brings up a new issue which is repeated bootstrapping. Lets say you have objects that
only need to be initialized __once__, but the bootstrap scene is on multiple BootstrapSequences.
The Bootstrapper has the option __Only bootstrap once__ which does exactly what it says, the
Bootstrapper's scene with the only bootstrap once flag set will be marked and the scene will never
be loaded again.

## Usage
  * Add a Bootstrapper component to any root GameObject in a scene, then set if the Bootstrapper is a
bootstrapping scene or not. 
  * Define that Bootstrapper's BootstrapSequence, even though it may not be used, see the above example.