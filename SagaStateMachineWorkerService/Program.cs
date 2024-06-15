using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Models;
using Shared;
using System.Reflection;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration configuration = context.Configuration;

        services.AddMassTransit(cfg =>
        {
            cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
               .EntityFrameworkRepository(opt =>
               {
                   opt.AddDbContext<DbContext, OrderStateDbContext>((provider, optionsBuilder) =>
                   {
                       optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), m =>
                       {
                           m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                       });
                   });
               });

            cfg.UsingRabbitMq((context, configure) =>
            {
                configure.Host(configuration.GetConnectionString("RabbitMQ"));
                configure.UseMessageRetry(x => x.Immediate(4));
                configure.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
                {
                    e.ConfigureSaga<OrderStateInstance>(context);
                });
            });
        });

        //services.AddMassTransitHostedService();
        services.AddHostedService<Worker>();
    });

var app = builder.Build();

app.Run();
