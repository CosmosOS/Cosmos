
; WSOCK32.DLL structures and constants

struct WSADATA
  wVersion       dw ?
  wHighVersion   dw ?
  szDescription  db 256+1 dup (?)
  szSystemStatus db 128+1 dup (?)
  iMaxSockets    dw ?
  iMaxUdpDg      dw ?
  lpVendorInfo   dd ?
ends

struct hostent
  h_name      dd ?
  h_aliases   dd ?
  h_addrtype  dw ?
  h_length    dw ?
  h_addr_list dd ?
ends

struct sockaddr_in
  sin_family dw ?
  sin_port   dw ?
  sin_addr   dd ?
  sin_zero   db 8 dup (?)
ends

struct sockaddr
  sa_family dw ?
  sa_data   db 14 dup (?)
ends

; Socket types

SOCK_STREAM    = 1
SOCK_DGRAM     = 2
SOCK_RAW       = 3
SOCK_RDM       = 4
SOCK_SEQPACKET = 5

; Address formats

AF_UNSPEC    = 0
AF_UNIX      = 1
AF_INET      = 2
AF_IMPLINK   = 3
AF_PUP       = 4
AF_CHAOS     = 5
AF_NS        = 6
AF_IPX       = 6
AF_ISO       = 7
AF_OSI       = AF_ISO
AF_ECMA      = 8
AF_DATAKIT   = 9
AF_CCITT     = 10
AF_SNA       = 11
AF_DECnet    = 12
AF_DLI       = 13
AF_LAT       = 14
AF_HYLINK    = 15
AF_APPLETALK = 16
AF_NETBIOS   = 17

; Protocol formats

PF_UNSPEC    = 0
PF_UNIX      = 1
PF_INET      = 2
PF_IMPLINK   = 3
PF_PUP       = 4
PF_CHAOS     = 5
PF_NS        = 6
PF_IPX       = 6
PF_ISO       = 7
PF_OSI       = PF_ISO
PF_ECMA      = 8
PF_DATAKIT   = 9
PF_CCITT     = 10
PF_SNA       = 11
PF_DECnet    = 12
PF_DLI       = 13
PF_LAT       = 14
PF_HYLINK    = 15
PF_APPLETALK = 16
PF_NETBIOS   = 17

; IP Ports

IPPORT_ECHO        = 7
IPPORT_DISCARD     = 9
IPPORT_SYSTAT      = 11
IPPORT_DAYTIME     = 13
IPPORT_NETSTAT     = 15
IPPORT_FTP         = 21
IPPORT_TELNET      = 23
IPPORT_SMTP        = 25
IPPORT_TIMESERVER  = 37
IPPORT_NAMESERVER  = 42
IPPORT_WHOIS       = 43
IPPORT_MTP         = 57
IPPORT_TFTP        = 69
IPPORT_RJE         = 77
IPPORT_FINGER      = 79
IPPORT_TTYLINK     = 87
IPPORT_SUPDUP      = 95
IPPORT_EXECSERVER  = 512
IPPORT_LOGINSERVER = 513
IPPORT_CMDSERVER   = 514
IPPORT_EFSSERVER   = 520
IPPORT_BIFFUDP     = 512
IPPORT_WHOSERVER   = 513
IPPORT_ROUTESERVER = 520
IPPORT_RESERVED    = 1024

; Notifications

FD_READ    = 01h
FD_WRITE   = 02h
FD_OOB     = 04h
FD_ACCEPT  = 08h
FD_CONNECT = 10h
FD_CLOSE   = 20h
