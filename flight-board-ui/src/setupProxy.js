const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function (app) {
    app.use(
        '/api', // Префикс для API-запросов
        createProxyMiddleware({
            target: 'http://localhost:7136', // Адрес бэкенда
            changeOrigin: true,
        })
    );
};