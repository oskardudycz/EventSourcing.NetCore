[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Implementing Distributed Processes

Added example of distributed processes management using Group Guests Checkout as an example.

It was modelled and explained in detail in the [Implementing Distributed Processes Webinar](https://www.architecture-weekly.com/p/webinar-3-implementing-distributed):

<a href="https://www.architecture-weekly.com/p/webinar-3-implementing-distributed" target="_blank"><img src="https://substackcdn.com/image/fetch/w_1920,h_1080,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-video.s3.amazonaws.com%2Fvideo_upload%2Fpost%2F69413446%2F526b9100-7271-4482-99e7-9559416e9848%2Ftranscoded-00624.png" alt="How to deal with privacy and GDPR in Event-Sourced systems" width="640" border="10" /></a>

It shows how to:
- orchestrate and coordinate business workflow spanning across multiple aggregates using [Saga pattern](https://event-driven.io/en/saga_process_manager_distributed_transactions/),
- handle distributed processing both for asynchronous commands scheduling and events publishing,
- getting at-least-once delivery guarantee,
- implementing command store and outbox pattern on top of Marten and EventStoreDB,
- unit testing aggregates and Saga with a little help from [Ogooreck](https://github.com/oskardudycz/Ogooreck),
- testing asynchronous code.

Read more in:
- [Saga and Process Manager - distributed processes in practice](https://event-driven.io/en/saga_process_manager_distributed_transactions/)
- [Event-driven distributed processes by example](https://event-driven.io/en/saga_process_manager_distributed_transactions/).

