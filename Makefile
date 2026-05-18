.PHONY: db-up db-down db-clean db-logs stack-up stack-down stack-logs

# Database only
db-up:
	docker compose up -d db

db-down:
	docker compose down

db-clean:
	docker compose down -v

db-logs:
	docker compose logs -f db

# Full stack
stack-up:
	docker compose up -d

stack-down:
	docker compose down

stack-logs:
	docker compose logs -f
