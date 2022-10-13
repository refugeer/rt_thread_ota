/*
 * File      : syswatch.h
 *
 * Change Logs:
 * Date           Author            Notes
 * 2020-01-10     qiyongzhong       first version
 */

#ifndef __SYSWATCH_H__
#define __SYSWATCH_H__

#include <syswatch_config.h>

#ifdef SYSWATCH_USING

typedef enum{
    SYSWATCH_EVENT_SYSTEM_RESET = 0,    //ϵͳ��λ�¼�,�¼�ǰ�ص�
    SYSWATCH_EVENT_THREAD_KILL,         //ɱ���߳��¼�,�¼�ǰ�ص�
    SYSWATCH_EVENT_THREAD_RESUMED,      //�ָ��߳��¼�,�¼���ص�
}syswatch_event_t;   

typedef void ( * syswatch_event_hook_t)(syswatch_event_t eid, rt_thread_t except_thread);

void syswatch_set_event_hook(syswatch_event_hook_t hook);//set syswatch event callback function
int syswatch_init(void);//Initialize syswatch components 

#endif  //SYSWATCH_USING
#endif  //__SYSWATCH_H__


