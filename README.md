# Discovering Healthier Recipes

With the rise of the internet, sharing recipes has never been easier. Sifting through the  billions of recipes online can be tedious and troublesome task, especially if your goal is to compare nutritional content. This project aims to fix this issue by providing an easy and robust method for consuming and comparing recipes at a large scale. 

This system can be broken up into 5 distinct sections: 

1. data collection: we utilize SQL bulk loading techniques and SQL CLR-computed columns to quickly gigabytes of recipes, nutrition, and ingredient substitution data.
2/3. Data parsing/ingredient matching: We use state-of-the-art machine learning techniques to parse and match this data together, allowing for easy access to every recipes' nutritional content.
4. Similarity measurement: we employs probabilistic similarity metrics to efficiently determine any number of similar recipes on a large scale, utilizing ingredient substitutions to boost our similarity measurements.
5. Recipe comparison: We utilize the flexibility of SQL to efficiently compare the nutrition between similar recipes.

Our results find that our system can ultimately discover healthier recipe alternatives in regards to many different nutritional measurements such as less sugar, more protein, and a higher NRF index ([https://pubmed.ncbi.nlm.nih.gov/20368382/](https://pubmed.ncbi.nlm.nih.gov/20368382/)). More information can be found in the attached report.
