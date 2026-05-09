/**
 * Remember to replace convertShapeToPath with the modified script.
 * Run the following commands with package manager console to build:
 * cd XVGO/wwwroot/js
 * npm init -y
 * npm install svgo esbuild
 * npx esbuild svg-optimizer.js --bundle --minify --outfile=./svg-optimizer.bundle.js --format=esm --platform=browser --alias:svgo=./node_modules/svgo/lib/svgo.js
 */

import { optimize } from 'svgo';

/**
 * @param {string} svgString
 * @param {boolean} forceMerge - If true, merges paths even if they overlap.
 * @returns {string}
 */
export function optimizeSvg(svgString, forceMerge = false)
{
    if (typeof svgString !== 'string' || !svgString.trim())
        return svgString;

    const SVGO_CONFIG = {
        js2svg: {
            indent: 2,
            pretty: true,
        },
        plugins: [
            'removeDoctype',
            'removeXMLProcInst',
            'removeComments',
            'removeDeprecatedAttrs',
            'removeMetadata',
            'removeEditorsNSData',
            'cleanupAttrs',
            'mergeStyles',
            {
                name: 'inlineStyles',
                params: {
                    onlyMatchedOnce: false
                }
            },
            'minifyStyles',
            'convertStyleToAttrs',
            'cleanupIds',
            'removeUselessDefs',
            'cleanupNumericValues',
            'convertOneStopGradients',
            'convertColors',
            'removeUnknownsAndDefaults',
            'removeNonInheritableGroupAttrs',
            'removeUselessStrokeAndFill',
            'cleanupEnableBackground',
            'removeHiddenElems',
            'removeEmptyText',
            {
                name: 'convertShapeToPath',
                params: {
                    convertArcs: true,
                    floatPrecision: 3
                }
            },
            'moveElemsAttrsToGroup',
            'moveGroupAttrsToElems',
            'collapseGroups',
            'convertPathData',
            'convertTransform',
            'removeEmptyAttrs',
            'removeEmptyContainers',
            {
                name: 'mergePaths',
                params: {
                    force: forceMerge,
                    floatPrecision: 3
                }
            },
            'removeUnusedNS',
            'sortAttrs',
            'sortDefsChildren',
            'removeDesc',
            'removeDimensions'
        ]
    };

    try
    {
        const result = optimize(svgString, SVGO_CONFIG);
        return result.data;
    }
    catch (err)
    {
        console.error('[SVG Optimizer] JS Error:', err);
        return svgString;
    }
}