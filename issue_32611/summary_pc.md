|                      | Win                                              | WSL                                              | Linux                                            |
|----------------------|--------------------------------------------------|--------------------------------------------------|--------------------------------------------------|
| mono                 | My hostname is 'manicka'                         | My hostname is 'manicka'                         | My hostname is 'manicka-pc'                      | 
|                      | My ip addresses are:                             | My ip addresses are:                             | My ip addresses are:                             |
|                      |   fe80::b982:2caf:4bc0:6dff%13                   |   192.168.0.103                                  |   192.168.0.103                                  |
|                      |   2a00:5c20:101:10a::7                           |                                                  |                                                  |
|                      |   192.168.0.103                                  |                                                  |                                                  |
| framework48          | My hostname is 'manicka'                         |                                                  |                                                  | 
|                      | My ip addresses are:                             |                                                  |                                                  |
|                      |   fe80::b982:2caf:4bc0:6dff%13                   |                                                  |                                                  | 
|                      |   2a00:5c20:101:10a::7                           |                                                  |                                                  | 
|                      |   192.168.0.103                                  |                                                  |                                                  |
| dotnet core          | My hostname is 'manicka'                         | My hostname is 'manicka'                         | My hostname is 'manicka-pc'                      | 
|                      | My ip addresses are:                             | My ip addresses are:                             | My ip addresses are:                             |
|                      |   fe80::b982:2caf:4bc0:6dff%13                   |   127.0.1.1                                      |   127.0.1.1                                      |
|                      |   2a00:5c20:101:10a::7                           |                                                  |                                                  | 
|                      |   192.168.0.103                                  |                                                  |                                                  |
| getaddrinfo          |   fe80::b982:2caf:4bc0:6dff                      |   127.0.1.1 (3x)                                 |   127.0.1.1 (3x)                                 |
|                      |   2a00:5c20:101:10a::7                           |                                                  |                                                  |
|                      |   192.168.0.103                                  |                                                  |                                                  |           
| getifaddrs/          |   fe80::5940:d69b:c568:aa44%2 (Ethernet 2)       |   192.168.0.103 (eth0)                           |   127.0.0.1 (lo)                                 |
| GetAdaptersAddresses |   169.254.170.68 (Ethernet 2)                    |   2a00:5c20:101:10a::7 (eth0)                    |   192.168.0.103 (enp3s0)                         |
|                      |   2a00:5c20:101:10a::7 (Ethernet)                |   fe80::b982:2caf:4bc0:6dff%eth0 (eth0)          |   ::1 (lo)                                       |
|                      |   fe80::b982:2caf:4bc0:6dff%13 (Ethernet)        |   169.254.170.68  (eth1)                         |   2a00:5c20:101:10a::4 (enp3s0)                  |
|                      |   192.168.0.103 (Ethernet)                       |   fe80::5940:d69b:c568:aa44%eth1 (eth1)          |   fe80::c79b:99ea:f48a:8f05%enp3s0 (enp3s0)      |
|                      |   ::1 (Loopback)                                 |   127.0.0.1 (lo)                                 |                                                  |
|                      |   127.0.0.1 (Loopback)                           |   ::1 (lo)                                       |                                                  |