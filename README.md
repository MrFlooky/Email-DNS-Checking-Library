# Simple DNS Checking Library

The most simple library to check, is email domain can handle emails or not. Developed using NS type of DNS.

# How to use

```
using EmailDnsChecker;
...
DnsResult result = await CheckEmailAsync("email@domain.com", 2);

// if result.Code > 0 - it is error, result is stored in result.Message
```
