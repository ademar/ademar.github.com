---
layout: page
title: Solving the riddle
---
{% include JB/setup %}

{% for post in site.posts %}
  {% assign post_year = post.date | date: "%Y" %}
  {% if post_year != current_year %}
    {% unless forloop.first %}
      </ul>
    {% endunless %}
    <h2 class="post-year">{{ post_year }}</h2>
    <ul class="posts">
    {% assign current_year = post_year %}
  {% endif %}
  <li>
    <time datetime="{{ post.date | date_to_xmlschema }}">{{ post.date | date: "%d %b" }}</time>
    <a href="{{ BASE_PATH }}{{ post.url }}">{{ post.title }}</a>
  </li>
  {% if forloop.last %}
    </ul>
  {% endif %}
{% endfor %}
