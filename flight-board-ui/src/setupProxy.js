const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function (app) {
    app.use(
        '/api', // ������� ��� API-��������
        createProxyMiddleware({
            target: 'http://localhost:7136', // ����� �������
            changeOrigin: true,
        })
    );
};