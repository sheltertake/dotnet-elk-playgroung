#!/bin/sh

# redirect stdout and stderr to files
exec >/out/results.txt
exec 2>/out/results.txt

# now run the requested CMD without forking a subprocess
exec "$@"