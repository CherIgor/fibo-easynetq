# Description

Fibonacci calculation using REST-api server, console-app client and message bus RabbitMQ via EasyNetQ. N Fibonacci sequence calculations are running in parallel at the same time.

## Installation

To install RabbitMQ locally in Docker use a command like:

```bash
docker run -d --hostname rabbitserver --name rabbitmq-server -p 15672:15672 -p 5672:5672 rabbitmq:3-management
```

See:
- https://www.rabbitmq.com/
- https://easynetq.com/

## Configuration
See [Fibo.FiboConsole/Config/Configuration.cs](https://github.com/CherIgor/fibo-easynetq/tree/main/Fibo.FiboConsole/Fibo.FiboConsole/Config) file for console-app configuration. Optionally pass number of parallel calculations via first argument to console app.

See [Fibo.FiboServer.App/appsettings.json](https://github.com/CherIgor/fibo-easynetq/tree/main/Fibo.FiboServer/Fibo.FiboServer.App) files for REST-api server configuration. Tune configuration depending on way of launching - locally or via docker-compose.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
