"""arq worker entrypoint (task 5.1).

Run with:  arq app.worker.settings.WorkerSettings
Requires Redis (see infra/docker-compose.yml).
"""
from arq import cron
from arq.connections import RedisSettings

from app.core.config import settings
from app.worker.tasks import enqueue_due_posts, publish_scheduled_post


class WorkerSettings:
    functions = [publish_scheduled_post]
    # Sweep for due posts twice a minute; each due post is enqueued as a job.
    cron_jobs = [cron(enqueue_due_posts, second={0, 30}, run_at_startup=True)]
    redis_settings = RedisSettings.from_dsn(settings.redis_url)
