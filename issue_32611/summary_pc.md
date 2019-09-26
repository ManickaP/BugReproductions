|             | Win                                              | Linux                                            |
|-------------|--------------------------------------------------|--------------------------------------------------|
| mono        | My hostname is 'manicka-pc'                      | My hostname is 'manicka-pc'                      | 
|             | My ip addresses are:                             | My ip addresses are:                             |
|             |                                                  |   192.168.0.103                                  |
| dotnet core | My hostname is 'manicka-pc'                      | My hostname is 'manicka-pc'                      | 
|             | My ip addresses are:                             | My ip addresses are:                             |
|             |                                                  |   127.0.1.1                                      |
| nslookup    |                                                  |   192.168.0.1                                    |
| getaddrinfo |                                                  |   127.0.1.1 (3x)                                 |
| getifaddrs  |                                                  |   127.0.0.1 (lo)                                 |
|             |                                                  |   192.168.0.103 (enp3s0)                         |
|             |                                                  |   ::1 (lo)                                       |
|             |                                                  |   2a00:5c20:101:10a::4 (enp3s0)                  |
|             |                                                  |   fe80::c79b:99ea:f48a:8f05%enp3s0 (enp3s0)      |