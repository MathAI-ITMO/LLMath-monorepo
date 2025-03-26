# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

# Skip type checking during build
RUN npm run build-only

# Production stage
FROM node:18-alpine

WORKDIR /app

COPY --from=builder /app/dist ./dist
COPY --from=builder /app/package*.json ./
COPY --from=builder /app/node_modules ./node_modules

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
  CMD ps aux | grep node || exit 1

EXPOSE 8080

# Use production mode
ENV NODE_ENV=production

# Start the application
CMD ["npm", "run", "preview", "--", "--host", "0.0.0.0", "--port", "8080"]