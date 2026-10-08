# Learn C# before writing a mod

If you have never programmed before, start by learning C#. Disharmony's examples assume you can read and write methods and classes. If you already can, continue with [How runtime patching works](runtime-patching.md).

## Start learning C#

Work through [Microsoft's C# for Beginners video series](https://learn.microsoft.com/en-us/shows/csharp-for-beginners/) in order. It starts with your first program and introduces variables, decisions, loops, and classes and objects.

Follow the setup instructions in the series, then write and run the examples yourself. Try changing a value or a condition and predict what the program will do before you run it. Practice until you can explain the result, rather than only reproduce the example.

Before following the Disharmony examples, practice until you can:

- Write and run a small C# program in your editor.
- Use variables to store numbers, text, and true/false values.
- Use an `if` statement to choose what happens, and a loop to repeat an operation.
- Write and call a method that takes parameters and returns a value.
- Write a class with a field or property, create an object of that class, and call one of its methods.
- Find the line indicated by a compiler error and correct a mistake in your code.

## Learn patching next

These skills prepare you to follow small patching examples. [How runtime patching works](runtime-patching.md) explains how game code and mod code work together, using a small method to introduce prefixes and postfixes.

Making a real mod also requires finding and understanding the game code responsible for the behavior you want to change. That often means using a decompiler to read the game code, learning from existing mods, and testing your changes in-game. This investigation can take longer than writing the patch itself.

Follow the patching introduction and [Getting started examples](obj/guides/getting-started.md) before continuing with [Plan a patch for your game](planning-a-game-patch.md), which takes you through that investigation.
