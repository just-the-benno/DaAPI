/// <binding Clean='clean' ProjectOpened='build-dev, watch-all' />
const sass = require('node-sass');

/// <binding Clean='clean:0' />
module.exports = function (grunt) {

    grunt.initConfig({
        clean:
        {
            assets: ["wwwroot/img/*", "wwwroot/icons/*", "wwwroot/fonts/*"],
            css: ["wwwroot/css/*"],
            wwwroot: ["wwwroot/css/*", "wwwroot/js/*", "wwwroot/img/*", "wwwroot/icons/*", "wwwroot/ts/*","wwwroot/fonts/*"],
            temp: ["FrontendAssets/temp/*"]
        },
        uglify: {
            ts: {
                options: {
                    mangle: {
                        reserved: ['jQuery']
                    }
                },
                files: [{
                    expand: true,
                    src: ['FrontendAssets/temp/ts/*.js', '!FrontendAssets/temp/ts/*.min.js'],
                    dest: '',
                    rename: function (dst, src) {
                        // To keep the source js files and make new files as `*.min.js`:
                        return src.replace('.js', '.min.js');
                    }
                }]
            }
        },
        watch: {
            scss: {
                files: 'FrontendAssets/scss/**/*.scss',
                tasks: ["clean:css", "sass", 'cssmin', "copy:css"]
            },
            assets:
            {
                files: ['FrontendAssets/icons/**', 'FrontendAssets/images/**', 'FrontendAssets/fonts/**'],
                tasks: ["clean:assets", "copy:assets"]
            },
            // ts:
            // {
            //     files: ['FrontendAssets/ts/**/*.ts'],
            //     tasks: ['uglify:ts', 'copy:ts', 'copy:ts_debug']
            // },
            tsOutput:
            {
                files: ['FrontendAssets/temp/**/*.js'],
                tasks: ['uglify:ts', 'copy:ts', 'copy:ts_debug']
            }
        },
        sass: {
            options: {
                implementation: sass,
                sourceMap: true
            },
            all: {
                files: {
                    'FrontendAssets/temp/css/app.css': 'FrontendAssets/scss/app.scss'
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
                    { expand: true, flatten: true, src: ['FrontendAssets/temp/css/**'], dest: 'wwwroot/css-app/', filter: 'isFile' }
                ]
            },
            assets: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/images/**'], dest: 'wwwroot/img-app/' },
                    { expand: true, flatten: true, src: ['FrontendAssets/icons/**'], dest: 'wwwroot/icons-app/', filter: 'isFile' },
                    { expand: true, flatten: true, src: ['FrontendAssets/fonts/**'], dest: 'wwwroot/fonts-app/', filter: 'isFile' },
                ]
            },
            js: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/temp/js/**'], dest: 'wwwroot/js-app/', filter: 'isFile' }
                ]
            },
            ts: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/temp/ts/**'], dest: 'wwwroot/js-app/', filter: 'isFile' }
                ]
            },
            ts_debug: {
                files: [
                    { expand: true, flatten: true, src: ['FrontendAssets/ts/**'], dest: 'wwwroot/ts-app/', filter: 'isFile' }
                ]
            },
            admin_lte:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/admin-lte/dist/css', src: '**', dest: 'wwwroot/css-app/adminlte3/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/admin-lte/dist/js', src: '*.js*', dest: 'wwwroot/js-app/adminlte3/', filter: 'isFile' }
                ]
            },
            fontawesome:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/css', src: '**', dest: 'wwwroot/css-app/plugins/fontawesome/css/', filter: 'isFile' },
                    { expand: true, flatten: false, cwd: 'node_modules/@fortawesome/fontawesome-free/webfonts', src: '**', dest: 'wwwroot/css-app/plugins/fontawesome/webfonts/', filter: 'isFile' },
                ]
            },
            icheck_bootstrap:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/icheck-bootstrap', src: '**', dest: 'wwwroot/css-app/plugins/icheck-bootstrap/', filter: 'isFile' }
                ]
            },
            bootstrap:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/bootstrap/dist/js', src: '**', dest: 'wwwroot/js-app/plugins/bootstrap/', filter: 'isFile' },
                ]
            },
            popper:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'FrontendAssets/custom-js-files/popper/', src: ['popper.min.js'], dest: 'wwwroot/js-app/plugins/popper/', filter: 'isFile' },
                ]
            },
            jquery:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/jquery/dist', src: '**', dest: 'wwwroot/js-app/plugins/jquery/', filter: 'isFile' },
                ]
            },
            moment:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/moment/min', src: '**', dest: 'wwwroot/js-app/plugins/moment/', filter: 'isFile' },
                ]
            },
            chartjs:
            {
                files: [
                    { expand: true, flatten: false, cwd: 'node_modules/chart.js/dist', src: '**', dest: 'wwwroot/js-app/plugins/chartjs/', filter: 'isFile' },
                ]
            }
        },
        ts: {
            default: {
                tsconfig: './tsconfig.json',
                options: {
                    fast: 'never'
                  }
            }
        },
        run: {
            typescript_watch:
            {
                options: {
                    wait: false
                },
                exec: 'tsc -w',
            }
        },
        concurrent: {
            watch: {
                tasks: ['watch', 'typescript-watch'],
                options: {
                    logConcurrentOutput: true
                }
            }
        }
    });

    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-sass');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks("grunt-ts");
    grunt.loadNpmTasks('grunt-run');
    grunt.loadNpmTasks('grunt-concurrent');

    grunt.registerTask("watch-all", ['concurrent:watch']);
    grunt.registerTask("build", [
        'clean:wwwroot', 'clean:temp',
        'ts', 'uglify:ts',
        'sass', 'cssmin',
        'copy:css', 'copy:assets', 'copy:js', 'copy:ts', 'copy:admin_lte', 'copy:fontawesome', 'copy:icheck_bootstrap', 'copy:bootstrap', 'copy:jquery', 'copy:popper', "copy:moment","copy:chartjs",
        'clean:temp']);

    grunt.registerTask("build-dev", [
        'clean:wwwroot', 'clean:temp',
        'ts', 'uglify:ts',
        'sass', 'cssmin',
        'copy:css', 'copy:assets', 'copy:js', 'copy:ts', 'copy:ts_debug', 'copy:fontawesome', 'copy:icheck_bootstrap', 'copy:bootstrap', 'copy:jquery', 'copy:popper', "copy:moment", "copy:chartjs",
        'clean:temp']);

    grunt.registerTask('typescript-watch', [
        'run:typescript_watch:keepalive'
    ]);
}; 