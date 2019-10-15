#include <stdio.h>
#include <stdlib.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <netdb.h>
#include <ifaddrs.h>
#include <sys/ioctl.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>

#define HOSTNAME_LEN 255

void main()
{
    // ----- getaddrinfo -----
    char *my_hostname = malloc(HOSTNAME_LEN);
    struct addrinfo hint;
    struct addrinfo *info = NULL;
    int result;
    char ipstr[INET6_ADDRSTRLEN];

    result = gethostname(my_hostname, HOSTNAME_LEN);
    if (result != 0)
    {
        printf("Error in gethostname() %d\n", result);
        return;
    }

    printf("My hostname is %s\n", my_hostname);

    memset(&hint, 0, sizeof(struct addrinfo));

    hint.ai_family = AF_UNSPEC;
    hint.ai_flags = AI_CANONNAME;

    result = getaddrinfo((const char *)my_hostname, NULL, &hint, &info);
    if (result != 0)
    {
        printf("Error in getaddrinfo() %d\n", result);
        return;
    }

    printf("%s\n", info->ai_canonname);
    printf("My IP addresses are:\n");
    for (struct addrinfo *ai = info; ai != NULL; ai = ai->ai_next)
    {
        void *addr;
        if (ai->ai_family == AF_INET) {
            addr = &(((struct sockaddr_in *)ai->ai_addr)->sin_addr);
        } else {
            addr = &(((struct sockaddr_in6 *)ai->ai_addr)->sin6_addr);
        }
        inet_ntop(ai->ai_family, addr, ipstr, sizeof(ipstr));
        printf("\t%s\n", ipstr);
    }
    // ----- getaddrinfo -----


    // ----- getifaddrs -----
    struct ifaddrs *ifaddr, *ifa;
    int family, s;
    char host[NI_MAXHOST];

    if (getifaddrs(&ifaddr) == -1)
    {
        perror("getifaddrs");
        exit(EXIT_FAILURE);
    }

    /* Walk through linked list, maintaining head pointer so we
       can free list later */

    for (ifa = ifaddr; ifa != NULL; ifa = ifa->ifa_next)
    {
        if (ifa->ifa_addr == NULL)
            continue;

        family = ifa->ifa_addr->sa_family;

        /* Display interface name and family (including symbolic
           form of the latter for the common families) */

        printf("%s  address family: %d%s\n",
               ifa->ifa_name, family,
               (family == AF_PACKET) ? " (AF_PACKET)" : (family == AF_INET) ? " (AF_INET)" : (family == AF_INET6) ? " (AF_INET6)" : "");

        /* For an AF_INET* interface address, display the address */

        if (family == AF_INET || family == AF_INET6)
        {
            void *addr;
            if (family == AF_INET) {
                addr = &(((struct sockaddr_in *)ifa->ifa_addr)->sin_addr);
            } else {
                addr = &(((struct sockaddr_in6 *)ifa->ifa_addr)->sin6_addr);
            }
            inet_ntop(family, addr, ipstr, sizeof(ipstr));

            /*s = getnameinfo(ifa->ifa_addr,
                            (family == AF_INET) ? sizeof(struct sockaddr_in) : sizeof(struct sockaddr_in6),
                            host, NI_MAXHOST, NULL, 0, NI_NUMERICHOST);
            if (s != 0)
            {
                printf("getnameinfo() failed: %s\n", gai_strerror(s));
                exit(EXIT_FAILURE);
            }*/
            printf("\taddress: <%s>\n", ipstr);
        }
    }

    freeifaddrs(NULL);
    freeifaddrs(ifaddr);
    exit(EXIT_SUCCESS);
    // ----- getifaddrs -----
}