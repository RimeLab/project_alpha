.PHONY: run migrate superuser shell install test clean app db-up db-down db-logs

# Default target
run:
	python manage.py runserver

# Start the database
db-up:
	docker compose up -d

# Stop the database
db-down:
	docker compose down

# View database logs
db-logs:
	docker compose logs -f db

# Apply migrations
migrate:
	python manage.py makemigrations
	python manage.py migrate

# Create a superuser
superuser:
	python manage.py createsuperuser

# Install dependencies
install:
	pip install -r requirements.txt

# Run tests
test:
	python manage.py test -v 2

