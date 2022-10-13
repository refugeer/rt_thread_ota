/*
 * File      : fal_cfg.h
 * This file is part of FAL (Flash Abstraction Layer) package
 * COPYRIGHT (C) 2006 - 2018, RT-Thread Development Team
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 *
 * Change Logs:
 * Date           Author       Notes
 * 2018-05-17     armink       the first version
 */

#ifndef _FAL_CFG_H_
#define _FAL_CFG_H_

#include <rtconfig.h>
#include <board.h>

#define NOR_FLASH_SPI_DEV_NAME              "nor_spi"

#define STM32_FLASH_START_ADRESS_16K        ((uint32_t)0x08000000) /* Base @ of Sector 0, 16 Kbytes */
#define FLASH_SIZE_GRANULARITY_16K          (4 * 16 * 1024)

#define STM32_FLASH_START_ADRESS_64K        ((uint32_t)0x08010000) /* Base @ of Sector 4, 64 Kbytes */
#define FLASH_SIZE_GRANULARITY_64K          (1 * 64 * 1024)

#define STM32_FLASH_START_ADRESS_128K       ((uint32_t)0x08020000) /* Base @ of Sector 5, 128 Kbytes */
#define FLASH_SIZE_GRANULARITY_128K         (3 * 128 * 1024)

/* ===================== Flash device Configuration ========================= */
extern const struct fal_flash_dev stm32_onchip_flash_128k;
extern struct fal_flash_dev nor_flash0;

/* flash device table */
#define FAL_FLASH_DEV_TABLE                                          \
{                                                                    \
    &stm32_onchip_flash_128k,                                           \
    &nor_flash0,                                                     \
}
/* ====================== Partition Configuration ========================== */
#ifdef FAL_PART_HAS_TABLE_CFG
/* partition table */
#define FAL_PART_TABLE                                                               \
{                                                                                    \
    {FAL_PART_MAGIC_WROD,  "app",     "onchip_flash_128k",                     0, 3* 128*1024, 0}, \
    {FAL_PART_MAGIC_WORD,  "data",     FAL_USING_NOR_FLASH_DEV_NAME,           0, 2*1024*1024, 0}, \
    {FAL_PART_MAGIC_WORD,  "cache",    FAL_USING_NOR_FLASH_DEV_NAME, 2*1024*1024, 1*1024*1024, 0}, \
    {FAL_PART_MAGIC_WORD,  "para",     FAL_USING_NOR_FLASH_DEV_NAME, 3*1024*1024, 2*1024*1024, 0}, \
    {FAL_PART_MAGIC_WORD,  "config",   FAL_USING_NOR_FLASH_DEV_NAME, 5*1024*1024, 1*1024*1024, 0}, \
    {FAL_PART_MAGIC_WORD,  "download", FAL_USING_NOR_FLASH_DEV_NAME, 6*1024*1024, 1*1024*1024, 0}, \
    {FAL_PART_MAGIC_WORD,  "factory",  FAL_USING_NOR_FLASH_DEV_NAME, 7*1024*1024, 1*1024*1024, 0}, \
}
#endif /* FAL_PART_HAS_TABLE_CFG */

#endif /* _FAL_CFG_H_ */
