from bs4 import BeautifulSoup
import bs4
from os import listdir
import os
from os.path import isfile, join
import pandas as pd
import re

local_file_path = os.path.join(os.getcwd(), 'foodsubs_scraping', 'scraped_pages')

files = [join(local_file_path, f) for f in listdir(local_file_path) if isfile(join(local_file_path, f))]

# notes tips substitutes pronunciation
# separated by =
# varieties? sometimes

subs_chunks = []
good_chunks_file = 'subs_webcontent_chunks.csv'
bad_chunks_file = 'bad_subs_webcontent_chunks.csv'

good_tups = []
bad_tups = []
for file in files:
    with open(file,  encoding='cp932', errors='ignore') as f:
        soup = BeautifulSoup(f,  'lxml')

    def contains_subs(elem):
        return (
                getattr(elem, 'name', None)  # is an element, not text
                # and any NavigableText child elements contain the word Cosmos
                and any('Substitutes' in child for child in elem.children
                        if not getattr(child, 'name', None))
        )
    def contains_subs2(elem):
        return (
                getattr(elem, 'name', None)  # is an element, not text
                # and any NavigableText child elements contain the word Cosmos
                and any('Substitutions' in child for child in elem.children
                        if not getattr(child, 'name', None))
        )

    subs_elements = soup.findAll(contains_subs)
    se2 = soup.findAll(contains_subs2)
    subs_elements = [s for s in subs_elements]
    subs_elements.extend([s for s in se2])
    subs_top = []
    for i in range(len(subs_elements)):
        subs_top.append(subs_elements[i].find_parent('td'))
    # subs_top = {sub_elem: sub_elem.find_parent('td') for sub_elem in subs_elements}
    subs_from = []
    subs_to = []
    for k in range(len(subs_elements)):
        subs_to.append(subs_elements[k].parent.next_sibling)
        if subs_top[k] is None:
            subs_top[k] = subs_elements[k].find_parent('p')
            if subs_elements[k].find_parent('p') is None:
                subs_top[k] = subs_elements[k].find_parent('td')
                if subs_top[k] is None:
                    subs_top[k] = subs_elements[k].find_parent('blockquote')

    for i in range(len(subs_top)):
        if subs_top[i]:
            first_bold = subs_top[i].find_all('b')
            if len(first_bold) > 0:
                subs_from.append(first_bold[0].getText().replace("\n", " "))
            else:
                subs_from.append(" ")
        else:
            subs_from.append(" ")

    subs_chunks.extend(subs_top)
    final_tups = []
    error_tups = []
    for s in range(len(subs_elements)):
        s_to = subs_to[s]

        s_from = subs_from[s].replace("\n", " ")
        s_from = s_from.replace(":", "")
        if s_from.find('Substitutes') > 0:
            s_from = s_from[:s_from.find('Substitutes')]
        if s_from.find('Notes') > 0:
            s_from = s_from[:s_from.find('Notes')]
        if s_from.find('Equivalents') > 0:
            s_from = s_from[:s_from.find('Equivalents')]
        if s_from.find('Pronunciation') > 0:
            s_from = s_from[:s_from.find('Pronunciation')]
        if s_from.find('Varieties') > 0:
            s_from = s_from[:s_from.find('Varieties')]
        if s_from.find('To make your own') > 0:
            s_from = s_from[:s_from.find('To make your own')]
        if s_from.find('Shopping hints') > 0:
            s_from = s_from[:s_from.find('Shopping hints')]


        if isinstance(s_to, bs4.element.NavigableString):
            s_to = s_to.replace("\n", " ")
        else:
            s_to = ""

        s_from = s_from.strip()
        s_to = s_to.strip()
        print(s_from)
        if s_from.replace(" ", "") == "" or s_to.replace(" ", "") == "" or\
                s_from == "Substitutes" or s_to == "Substitutes" or s_from == "Tips" or s_to == "Tips":
            error_tups.append((str(s_from),
                               str(s_to),
                               file))
        else:
            for s_split_from in re.split('=| or | OR',str(s_from)):
                s_split_from = s_split_from.strip()
                if s_split_from and not str.isspace(s_split_from):
                    for s_split_to in re.split('=| or | OR',str(s_to)):
                        s_split_to = s_split_to.strip()
                        if s_split_to and not str.isspace(s_split_to):
                            final_tups.append((s_split_from, s_split_to, file))

    good_tups.extend(final_tups)
    bad_tups.extend(error_tups)


print('good substitute tuples: ', len(good_tups))
print('bad substitutes: ', len(bad_tups))

df = pd.DataFrame(good_tups, columns=[ 'Substitution1', 'Substitution2', 'Webpage' ])
# print(df.Substitution1.str.len().max())
# print(df.Substitution2.str.len().max())
# print(df.Webpage.str.len().max())
df.to_csv(good_chunks_file, index=False)

df = pd.DataFrame(bad_tups, columns=[ 'Substitution1', 'Substitution2', 'Webpage' ])
df.to_csv(bad_chunks_file, index=False)