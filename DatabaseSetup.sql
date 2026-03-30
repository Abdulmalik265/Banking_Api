-- Banking API Database Setup
-- Run this script in MySQL to create the database and tables

CREATE DATABASE IF NOT EXISTS banking_api;
USE banking_api;

CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20) DEFAULT '',
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    UNIQUE INDEX ix_users_email (email)
);

CREATE TABLE IF NOT EXISTS accounts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    account_number VARCHAR(10) NOT NULL,
    balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    currency VARCHAR(5) NOT NULL DEFAULT 'NGN',
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    UNIQUE INDEX ix_accounts_account_number (account_number),
    CONSTRAINT fk_accounts_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    reference VARCHAR(50) NOT NULL,
    sender_id INT NOT NULL,
    recipient_id INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    sender_balance_before DECIMAL(18,2) NOT NULL,
    sender_balance_after DECIMAL(18,2) NOT NULL,
    recipient_balance_before DECIMAL(18,2) NOT NULL,
    recipient_balance_after DECIMAL(18,2) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Successful',
    narration VARCHAR(200) NULL,
    created_at DATETIME NOT NULL,
    UNIQUE INDEX ix_transactions_reference (reference),
    CONSTRAINT fk_transactions_sender FOREIGN KEY (sender_id) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_transactions_recipient FOREIGN KEY (recipient_id) REFERENCES users(id) ON DELETE RESTRICT
);
