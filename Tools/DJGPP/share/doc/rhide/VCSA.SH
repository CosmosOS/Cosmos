#!/bin/bash
#
# This script can be used to create a new group
# called 'vcsa' and change the goup owner of the
# vcsa devices to 'vcsa' and setting rw permission
# to that devices for the group vcsa.
#
# You can use this, if you don't want to have RHIDE
# and it's related programs to be a root setuid program.
# In that case you can make them a vcsa setgid program.

# create the new group
groupadd vcsa

for i in /dev/vcsa[1-9] /dev/vcsa[1-9][0-9]
do
  echo "chgrp vcsa $i; chmod 660 $i"
  chgrp vcsa $i; chmod 660 $i
done


