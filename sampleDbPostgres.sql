-- Create Users Table
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL,
    password VARCHAR(255) NOT NULL,
    role INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Addresses Table
CREATE TABLE addresses (
    address_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    street VARCHAR(255) NOT NULL,
    city VARCHAR(100) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Categories Table
CREATE TABLE categories (
    category_id SERIAL PRIMARY KEY,
    category_name VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Products Table
CREATE TABLE products (
    product_id SERIAL PRIMARY KEY,
    product_name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    category_id INT REFERENCES categories(category_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Orders Table
CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    order_date DATE NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Order Items Table
CREATE TABLE order_items (
    order_item_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id),
    product_id INT REFERENCES products(product_id),
    quantity INT NOT NULL,
    item_price DECIMAL(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Reviews Table
CREATE TABLE reviews (
    review_id SERIAL PRIMARY KEY,
    product_id INT REFERENCES products(product_id),
    user_id INT REFERENCES users(user_id),
    rating INT NOT NULL,
    comment TEXT,
    review_date DATE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Trigger to Update updated_at on Update
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Attach Trigger to Tables
CREATE TRIGGER update_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_addresses_updated_at
BEFORE UPDATE ON addresses
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_categories_updated_at
BEFORE UPDATE ON categories
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_products_updated_at
BEFORE UPDATE ON products
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_orders_updated_at
BEFORE UPDATE ON orders
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_order_items_updated_at
BEFORE UPDATE ON order_items
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_reviews_updated_at
BEFORE UPDATE ON reviews
FOR EACH ROW
EXECUTE FUNCTION update_updated_at();


-------data
-- Insert Sample Data into Users Table
INSERT INTO users (username, email, password, role) VALUES
    ('john_doe', 'john@example.com', 'password123', 0),
    ('jane_doe', 'jane@example.com', 'securepass', 0),
    ('alice_smith', 'alice@example.com', 'pass123', 0),
    ('admin', 'admin@example.com', 'admin', 1);

-- Insert Sample Data into Addresses Table
INSERT INTO addresses (user_id, street, city, postal_code) VALUES
    (1, '123 Main St', 'Anytown', '12345'),
    (2, '456 Oak St', 'Sometown', '54321'),
    (3, '789 Elm St', 'Othercity', '67890'),
    (4, '101 Pine St', 'Anothercity', '45678');

-- Insert Sample Data into Categories Table
INSERT INTO categories (category_name) VALUES
    ('Electronics'),
    ('Clothing'),
    ('Books'),
    ('Furniture'),
    ('Toys'),
    ('Sporting Goods');

-- Insert Sample Data into Products Table
INSERT INTO products (product_name, price, category_id) VALUES
    ('Smartphone', 499.99, 1),
    ('T-shirt', 19.99, 2),
    ('Programming Book', 29.99, 3),
    ('Coffee Table', 149.99, 4),
    ('Board Game', 24.99, 5),
    ('Running Shoes', 79.99, 6);

-- Insert Sample Data into Orders Table
INSERT INTO orders (user_id, order_date, total_amount) VALUES
    (1, '2023-01-01', 100.00),
    (2, '2023-01-02', 150.50),
    (3, '2023-01-03', 75.25),
    (4, '2023-01-04', 200.00);

-- Insert Sample Data into Order Items Table
INSERT INTO order_items (order_id, product_id, quantity, item_price) VALUES
    (1, 1, 1, 499.99),
    (2, 2, 2, 39.98),
    (3, 3, 1, 29.99),
    (4, 4, 1, 149.99);

-- Insert Sample Data into Reviews Table
INSERT INTO reviews (product_id, user_id, rating, comment, review_date) VALUES
    (1, 1, 5, 'Great phone!', '2023-01-02'),
    (2, 2, 4, 'Nice shirt, fits well.', '2023-01-03'),
    (3, 3, 4, 'Sturdy book, good content.', '2023-01-04'),
    (4, 4, 5, 'Quality coffee table.', '2023-01-05');

