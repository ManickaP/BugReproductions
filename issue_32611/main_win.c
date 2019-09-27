#include <stdio.h>
#include <stdlib.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
#include <iphlpapi.h>

#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0600
#endif

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "iphlpapi.lib")

#define HOSTNAME_LEN 255


void print_adapter(PIP_ADAPTER_ADDRESSES aa)
{
	char buf[BUFSIZ];
	memset(buf, 0, BUFSIZ);
	WideCharToMultiByte(CP_ACP, 0, aa->FriendlyName, wcslen(aa->FriendlyName), buf, BUFSIZ, NULL, NULL);
	printf("adapter_name:%s\n", buf);
}

void print_addr(PIP_ADAPTER_UNICAST_ADDRESS ua)
{
	char buf[BUFSIZ];

	int family = ua->Address.lpSockaddr->sa_family;
	printf("\t%s ",  family == AF_INET ? "IPv4":"IPv6");

	memset(buf, 0, BUFSIZ);
	getnameinfo(ua->Address.lpSockaddr, ua->Address.iSockaddrLength, buf, sizeof(buf), NULL, 0,NI_NUMERICHOST);
	printf("%s\n", buf);	
}

int print_ipaddress()
{
	DWORD rv, size;
	PIP_ADAPTER_ADDRESSES adapter_addresses, aa;
	PIP_ADAPTER_UNICAST_ADDRESS ua;

	rv = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_INCLUDE_PREFIX, NULL, NULL, &size);
	if (rv != ERROR_BUFFER_OVERFLOW) {
		fprintf(stderr, "GetAdaptersAddresses() failed...");
		return 0;
	}
	adapter_addresses = (PIP_ADAPTER_ADDRESSES)malloc(size);

	rv = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_INCLUDE_PREFIX, NULL, adapter_addresses, &size);
	if (rv != ERROR_SUCCESS) {
		fprintf(stderr, "GetAdaptersAddresses() failed...");
		free(adapter_addresses);
		return 0;
	}

	for (aa = adapter_addresses; aa != NULL; aa = aa->Next) {
		print_adapter(aa);
		for (ua = aa->FirstUnicastAddress; ua != NULL; ua = ua->Next) {
			print_addr(ua);
		}
	}

	free(adapter_addresses);
    return 1;
}


void main()
{
    WSADATA wsa;	
	printf("\nInitialising Winsock...");
	if (WSAStartup(MAKEWORD(2,2),&wsa) != 0)
	{
		printf("Failed. Error Code : %d",WSAGetLastError());
		return 1;
	}

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

    printf("My IP addresses are:\n");
    for (struct addrinfo *ai = info; ai != NULL; ai = ai->ai_next)
    {
        void *addr = ai->ai_family == AF_INET ? &(((struct sockaddr_in *)ai->ai_addr)->sin_addr) : &(((struct sockaddr_in6 *)ai->ai_addr)->sin6_addr);
        inet_ntop(ai->ai_family, addr, ipstr, sizeof(ipstr));
        printf("\t%s\n", ipstr);
    }
    // ----- getaddrinfo -----

    // ----- getifaddrs ----- 
    print_ipaddress();
	return 0;
    // ----- getifaddrs -----
}