#!/usr/bin/env python3
'''Sample command-line client for LgTvControl REST API.'''

import os,sys,requests

def url(*parts):
    return requests.urllib3.util.Url(
        scheme='http',
        host='localhost',
        port=3840,
        path=os.path.join(*(['LgTv'] + list(parts))))

def get(setting):
    r = requests.get(url(setting))
    r.raise_for_status()
    return r.json()

def put(setting, value):
    requests.put(url(setting), json=value).raise_for_status()

settings=('Backlight','Input','OnScreenDisplay','Power')

def parse_setting(s):
    names = [i for i in settings if i.lower().startswith(s.lower())]
    if len(names) == 1:
        return names[0]
    else:
        raise ValueError

usage=f'''
lgtvctl {'|'.join(settings)} 
lgtvctl {'|'.join(settings)} toggle|on|off|true|false|[+|-]<Value>
'''.strip()

def _main(*args):
    if len(args) == 1:
        setting = parse_setting(args[0])
        print(get(setting))
    elif len(args) == 2:
        setting = parse_setting(args[0])
        valstr = args[1].lower()
        if valstr == 'toggle':
            put(setting, not get(setting))
        elif valstr.startswith('+') or valstr.startswith('-'):
            put(setting, max(0, min(get(setting) + int(valstr), 100)))
        else:
            if valstr in ('true','on'):
                value = True
            elif valstr in ('false','off'):
                value = False
            else:
                value = int(valstr)
            put(setting, value)
    else:
        raise ValueError

def main(*args):
    try:
        _main(*args)
    except ValueError:
        print(usage, file=sys.stderr)
        return 2
    except requests.HTTPError as e:
        print(e, file=sys.stderr)
        return 1
    else:
        return 0

if os.path.basename(__file__) != None and __name__ == '__main__':
    sys.exit(main(*sys.argv[1:]))
