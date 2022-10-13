/*
 * Copyright (c) 2006-2022, RT-Thread Development Team
 *
 * SPDX-License-Identifier: Apache-2.0
 *
 * Change Logs:
 * Date           Author       Notes
 * 2022-10-11     RT-Thread    first version
 */

#include <rtthread.h>
#include <board.h>

#define DBG_TAG "main"
#define DBG_LVL DBG_LOG
#include <rtdbg.h>

#define APP_VERSION "1.0.2"
/**
 * Function    ota_app_vtor_reconfig
 * Description Set Vector Table base location to the start addr of app(RT_APP_PART_ADDR).
*/
static int ota_app_vtor_reconfig(void)
{
    #define NVIC_VTOR_MASK     0x3FFFFF80
    #define RT_APP_PART_ADDR   0x8020000
    /* Set the Vector Table base location by user application firmware definition */
    SCB->VTOR = RT_APP_PART_ADDR & NVIC_VTOR_MASK;

    return 0;    rt_kprintf("this is the second test firmware version 1.0.2 from refugeer@2022-10-12 \n");
}
INIT_BOARD_EXPORT(ota_app_vtor_reconfig);

#include <fal.h>
extern int fal_init(void);
INIT_COMPONENT_EXPORT(fal_init);

int main(void)
{
    int count = 1;

    rt_kprintf("the current version of APP firmware is %s\n", APP_VERSION);
    rt_kprintf("this is the second test firmware version 1.0.2 from refugeer@2022-10-12 \n");

    while (count++)
    {
        rt_thread_mdelay(1000);
    }

    return RT_EOK;
}
