const path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const outputDir = 'wwwroot/dist';

module.exports = (env, argv) => {
    return {
        mode: process.env.NODE_ENV,
        entry: './wwwroot/styles.css',
        output: {
            path: path.resolve(__dirname, outputDir)
        },
        module: {
            rules: [{
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    { loader: "css-loader", options: { importLoaders: 1 } },
                    'postcss-loader'
                ]
            }]
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: 'styles.css'
            })
        ]
    };
};
