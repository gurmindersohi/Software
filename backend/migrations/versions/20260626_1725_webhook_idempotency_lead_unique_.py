"""webhook idempotency + lead unique constraint

Revision ID: 2b1e6104d729
Revises: db098b67a847
Create Date: 2026-06-26 17:25:40.849777
"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa
import sqlmodel


revision: str = '2b1e6104d729'
down_revision: Union[str, None] = 'db098b67a847'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        'processed_webhook_events',
        sa.Column('id', sqlmodel.sql.sqltypes.AutoString(), nullable=False),
        sa.Column('created_on', sa.DateTime(), nullable=False),
        sa.PrimaryKeyConstraint('id'),
    )
    # batch mode so SQLite (copy-and-move) and Postgres (plain ALTER) both work
    with op.batch_alter_table('leads', schema=None) as batch_op:
        batch_op.create_unique_constraint('uq_lead_account_email', ['account_id', 'email'])


def downgrade() -> None:
    with op.batch_alter_table('leads', schema=None) as batch_op:
        batch_op.drop_constraint('uq_lead_account_email', type_='unique')
    op.drop_table('processed_webhook_events')
