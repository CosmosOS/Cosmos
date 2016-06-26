/*
 * getopt.c
 *
 * Replacement for a Unix style getopt function
 *
 * Quick, clean, and portable to funky systems that don't have getopt() 
 * for whatever reason.
 *
 */

//#include <stdio.h>
//#include <string.h>

//#ifndef MSDOS_16BIT
//#define cdecl
//#endif

//int optind = 1;
//int optopt = 0;

//const char *optarg = NULL;

//int cdecl getopt (int argc, char *argv[], const char *options)
//{
//    static int pos = 1;

//    const char *p;

//    if (optind >= argc || argv[optind][0] != '-' || argv[optind][1] == 0)
//    return EOF;

//    optopt = argv[optind][pos++];
//    optarg = NULL;

//    if (argv[optind][pos] == 0)
//    { pos = 1; optind++; }

//    p = strchr (options, optopt);

//    if (optopt == ':' || p == NULL) {

//    fputs ("illegal option -- ", stdout);
//    goto error;

//    } else if (p[1] == ':') {

//    if (optind >= argc) {

//        fputs ("option requires an argument -- ", stdout);
//        goto error;

//    } else {

//        optarg = argv[optind];

//        if (pos != 1)
//        optarg += pos;

//        pos = 1; optind++;

//    }
//    }
//    return optopt;

//error:

//    fputc (optopt, stdout);
//    fputc ('\n', stdout);

//    return '?';

//}/* getopt */
