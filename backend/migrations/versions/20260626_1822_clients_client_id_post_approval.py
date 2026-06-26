"""clients + client_id + post approval

Revision ID: ca8c13bf0ae9
Revises: 2b1e6104d729
Create Date: 2026-06-26 18:22:31.593886
"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa
import sqlmodel


revision: str = 'ca8c13bf0ae9'
down_revision: Union[str, None] = '2b1e6104d729'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        'clients',
        sa.Column('id', sa.Uuid(), nullable=False),
        sa.Column('account_id', sa.Uuid(), nullable=False),
        sa.Column('name', sqlmodel.sql.sqltypes.AutoString(), nullable=False),
        sa.Column('is_deleted', sa.Boolean(), nullable=False),
        sa.Column('created_on', sa.DateTime(), nullable=False),
        sa.ForeignKeyConstraint(['account_id'], ['accounts.id']),
        sa.PrimaryKeyConstraint('id'),
    )
    op.create_index(op.f('ix_clients_account_id'), 'clients', ['account_id'], unique=False)

    # client_id FK columns — batch mode so SQLite (rebuild) + Postgres both work.
    for table in ('ad_accounts', 'leads', 'social_media_accounts', 'users'):
        with op.batch_alter_table(table) as batch:
            batch.add_column(sa.Column('client_id', sa.Uuid(), nullable=True))
            batch.create_index(op.f(f'ix_{table}_client_id'), ['client_id'], unique=False)
            batch.create_foreign_key(f'fk_{table}_client', 'clients', ['client_id'], ['id'])

    with op.batch_alter_table('scheduled_posts') as batch:
        batch.add_column(sa.Column('client_id', sa.Uuid(), nullable=True))
        batch.add_column(
            sa.Column('requires_approval', sa.Boolean(), nullable=False, server_default=sa.false())
        )
        batch.add_column(
            sa.Column(
                'approval_status',
                sqlmodel.sql.sqltypes.AutoString(),
                nullable=False,
                server_default='approved',
            )
        )
        batch.create_index(
            op.f('ix_scheduled_posts_approval_status'), ['approval_status'], unique=False
        )
        batch.create_index(op.f('ix_scheduled_posts_client_id'), ['client_id'], unique=False)
        batch.create_foreign_key('fk_scheduled_posts_client', 'clients', ['client_id'], ['id'])


def downgrade() -> None:
    with op.batch_alter_table('scheduled_posts') as batch:
        batch.drop_column('approval_status')
        batch.drop_column('requires_approval')
        batch.drop_column('client_id')
    for table in ('ad_accounts', 'leads', 'social_media_accounts', 'users'):
        with op.batch_alter_table(table) as batch:
            batch.drop_column('client_id')
    op.drop_index(op.f('ix_clients_account_id'), table_name='clients')
    op.drop_table('clients')
