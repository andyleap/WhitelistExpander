# WhitelistExpander

Whitelist expander provides a simple way to add to the SpaceEngineers mod whitelist.  On it's first run, it will create a "ModWhitelist" directory in the same directory as itself.
To add a type to the whitelist, simply create a text file in the ModWhitelist directory, containing the type's full namespace and name, followed by either a member to whitelist, or a *
to whitelist the entire type.

Some examples:
```
System.Environment.CurrentManagedThreadId
```
```
System.NotSupportedException.*
```
