docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management

guest guest

wget https://raw.githubusercontent.com/grafana/loki/v2.3.0/production/docker-compose.yaml -O docker-compose.yaml
docker-compose -f docker-compose.yaml up

admin admin





count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[30m])

sum(count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[30m])) by (userName)

// ����������Կ���, ��variationΪĳ��ֵʱ��ʹ�õ��û�����
count(count by (userName) (count_over_time({featureFlagId="ff__2__3__a1",varaition="false"}[1d])))

// ����������Կ���, ����variationʱ��ʹ�õ��û�����
count(count by (userName) (count_over_time({featureFlagId="ff__2__3__a1"}[1d])))

// ĳ��ff��ĳ��״̬�µĵ������
{featureFlagId="ff__2__3__a1",varaition="false"}

// ����ֵ�ͼƬ
sum(count_over_time({featureFlagId="ff__2__3__a1"}[3h])) without(userName, varaition) 





1629006198000000000
1570818238000000000
1629009161000000000