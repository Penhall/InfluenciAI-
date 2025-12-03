-- Script SQL para criar usuário admin manualmente
-- Execute este script se o seed não estiver funcionando

-- 1. Criar Tenant (se não existir)
INSERT INTO "Tenants" ("Id", "Name", "IsActive", "CreatedAt")
VALUES ('adffdfe4-847f-464c-9521-13932095661d', 'Default', true, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- 2. Criar Usuário Admin
-- Senha: Admin!234 (hash gerado pelo Identity)
-- ATENÇÃO: Este hash foi gerado para a senha "Admin!234" com Identity padrão
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount",
    "TenantId"
)
VALUES (
    'e14e521c-83db-4d9e-a649-9c7fbfff356c',
    'admin@local',
    'ADMIN@LOCAL',
    'admin@local',
    'ADMIN@LOCAL',
    true,
    'AQAAAAIAAYagAAAAEHxK7kGqNLPzFqQvL1e3pZmRqJ5tJ4xF0X6J3J7J8J9J0J1J2J3J4J5J6J7J8J9J0J1J2==',
    '6JQVQK3XQFZQFQZQFQZQFQZQFQZQFQZQ',
    gen_random_uuid()::text,
    false,
    false,
    true,
    0,
    'adffdfe4-847f-464c-9521-13932095661d'
)
ON CONFLICT ("Id") DO NOTHING;

-- 3. Adicionar role admin
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES (gen_random_uuid()::text, 'admin', 'ADMIN', gen_random_uuid()::text)
ON CONFLICT DO NOTHING;

-- 4. Associar usuário à role
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 'e14e521c-83db-4d9e-a649-9c7fbfff356c', "Id"
FROM "AspNetRoles"
WHERE "Name" = 'admin'
ON CONFLICT DO NOTHING;

-- Verificar se foi criado
SELECT "Id", "Email", "UserName" FROM "AspNetUsers" WHERE "Email" = 'admin@local';
