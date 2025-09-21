docker run -d --name rabbitmq `
  -p 5672:5672 -p 15672:15672 `
  -e RABBITMQ_DEFAULT_USER=guest `
  -e RABBITMQ_DEFAULT_PASS=guest `
  -e RABBITMQ_DEFAULT_VHOST=/ `
  -v rabbit-data:/var/lib/rabbitmq `
  rabbitmq:3-management