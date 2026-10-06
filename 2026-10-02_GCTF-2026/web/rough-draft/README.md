# Rough Draft - GCTF 2026
Solved by [`SelinaTan`](https://github.com/SelinaTan05)

## The Solve

First, going to `robots.txt` to find any hidden endpoints.

![alt text](image.png)
![alt text](image-1.png)

Found `Vite 6.2.2` configuration:

![alt text](image-2.png)

Check security issue of `Vite 6.2.2` configuration

![alt text](image-3.png)

Try reading .env file and get the flag:
`gctf26{c5bb403583904f17aeddf51e1fd50ee8a4b74dc9d6fe80272b464c0e81b635e8} `
![alt text](image-4.png)