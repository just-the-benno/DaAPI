/// <binding Clean='clean' ProjectOpened='build-dev, watch-all' />
const sass = require('node-sass');

/// <binding Clean='clean:0' />
module.exports = function (grunt) {

    grunt.initConfig({
        clean:
        {
            assets: ["wwwroot/img/*", "wwwroot/icons/*", "wwwroot/fonts/*"],
            css: ["wwwroot/css/*"],
            wwwroot: ["wwwroot/css/*", "wwwroot/js/*", "wwwroot/img/*", "wwwroot/icons/*", "wwwroot/ts/*", "wwwroot/fonts/*"],
            temp: ["FrontendAssets/temp/*"],
            tempjs: ["FrontendAssets/temp/js/*"]
        },
        uglify: {
            js:
            {
                expand: true,
                cwd: 'FrontendAssets/temp/js/',
                src: '**/*.js',
                dest: 'FrontendAssets/temp/js/',
                rename: function (destBase, destPath) {
                    return destBase + destPath.replace('.js', '.min.js');
                }
            },
        },
        watch: {
            scss: {
                files: 'FrontendAssets/scss/**/*.scss',
                tasks: ["clean:css", "sass", 'cssmin', "copy:css"]
            },
            js: {
                files: 'FrontendAssets/js/**/*.js',
                tasks: ["clean:tempjs", "copy:jstemp", "uglify:js", "copy:js"]
            },
            assets:
            {
                files: ['FrontendAssets/icons/**', 'FrontendAssets/images/**', 'FrontendAssets/fonts/**'],
                tasks: ["clean:assets", "copy:assets"]
            },
        },
        sass: {
            options: {
                implementation: sass,
                sourceMap: true
            },
            all: {
                files: {
                    'FrontendAssets/temp/css/main.css': 'FrontendAssets/scss/main.scss'
                }
            }
        },
        cssmin: {
            all: {
                files: [{
                    expand: true,
                    cwd: 'FrontendAssets/temp/css/',
                    src: ['*.css', '!*.min.css'],
                    dest: 'FrontendAssets/temp/css/',
                    ext: '.min.css'
                }]
            }
        },
        copy: {
            css: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/temp/css/**'], dest: 'wwwroot/css/', filter: 'isFile' }
                ]
            },
            assets: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/images/**'], dest: 'wwwroot/img/' },
                    { expand: true, flatten: true, src: ['FrontendAssets/icons/**'], dest: 'wwwroot/icons/', filter: 'isFile' },
                    { expand: true, flatten: true, src: ['FrontendAssets/fonts/**'], dest: 'wwwroot/fonts/', filter: 'isFile' },
                ]
            },
            jstemp:
            {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/js/**'], dest: 'FrontendAssets/temp/js/', filter: 'isFile' }
                ]
            },
            js: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/temp/js/**'], dest: 'wwwroot/js/', filter: 'isFile' }
                ]
            },
            admin_lte:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/admin-lte/dist/css', src: '**', dest: 'wwwroot/css/adminlte3/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/admin-lte/dist/js', src: '*.js*', dest: 'wwwroot/js/adminlte3/', filter: 'isFile' }
                ]
            },
            fontawesome:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/css', src: '**', dest: 'wwwroot/css/plugins/fontawesome/css/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/webfonts', src: '**', dest: 'wwwroot/css/plugins/fontawesome/webfonts/', filter: 'isFile' },
                ]
            },
            icheck_bootstrap:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/icheck-bootstrap', src: '**', dest: 'wwwroot/css/plugins/icheck-bootstrap/', filter: 'isFile' }
                ]
            },
            bootstrap:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/bootstrap/dist/js', src: '**', dest: 'wwwroot/js/plugins/bootstrap/', filter: 'isFile' },
                ]
            },
            jquery:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/jquery/dist', src: '**', dest: 'wwwroot/js/plugins/jquery/', filter: 'isFile' },
                ]
            }
        },
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-sass');
    grunt.loadNpmTasks('grunt-contrib-cssmin');

    grunt.registerTask("watch-all", ['watch']);
    grunt.registerTask("build", [
        'clean:wwwroot', 'clean:temp',
        'copy:jstemp', 'uglify:js',
        'sass', 'cssmin',
        'copy:css', 'copy:assets', 'copy:js', 'copy:admin_lte', 'copy:fontawesome', 'copy:icheck_bootstrap', 'copy:bootstrap', 'copy:jquery',
        'clean:temp']);

    grunt.registerTask("build-dev", [
        'clean:wwwroot', 'clean:temp',
        'ts', 'uglify:ts',
        'concat', 'uglify:js',
        'sass', 'cssmin',
        'copy:css', 'copy:assets', 'copy:js', 'copy:ts', 'copy:ts_debug', 'copy:fontawesome', 'copy:icheck_bootstrap', 'copy:bootstrap', 'copy:jquery',
        'clean:temp']);
}; 