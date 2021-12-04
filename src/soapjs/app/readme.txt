- components is in a subfolder so that when you use an alias to set @soap/modules to another directory it will find the node_modules of its parent, otherwise it will install a second copy of react.
- annoyingly some parcel features like inlining fs are only available when not using library mode but the docs don't say that.. grr.
- i still can't understand why .parcel-cache is always created in the app folder, even when i run it from components folder with the package.json from the app folder deleted!? i have had so many problems with components folder builds reading
 the parent app folder (package.json, .env) this makes me nervous but it does seem to work ok 
 - npx http-server -p 1234 -a localhost to test prod builds locally, for some reason scope hosting doesn't work when building from the soap package vs source, needs investigation
 - you could probably do much better and just alias straight to the source and solve it that way better than all this mess. something else to try.