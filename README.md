# Call-n-Cache
An API service that calls a configured endpoint and caches the result.

# Current Limitations
- Requests are a Post with JSON body
- Results expected as JSON

# Environment Variables
```
Uri: https://my.url.com
Headers__API-Key: token
Headers__X-API-KEY: token
Base64Payload: eyJoZWxsbyI6IndvcmxkISJ9
ShowException: false
```