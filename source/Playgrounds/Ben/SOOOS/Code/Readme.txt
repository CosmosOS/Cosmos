This service contains all the global system locks  ( Mutexes , Semephores , Waithandle ( AutoReset , ManualReset) )

It can be queried via the normal IPC system ( and thats how all non default locks are created) but trusted code will skip IPC for direct RPC access ( for performance reasons) 
hence these locks are memory safe  with immutable struct wrappers.

This also allows us to persist global locks and monitor them easily. 